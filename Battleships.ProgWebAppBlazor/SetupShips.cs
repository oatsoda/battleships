using Battleships.GameEngine;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Battleships.ProgWebAppBlazor
{
    public class SetupShips
    {
        private const string EXPRESSION = "^[A-Ja-j]{1}[0-9]{1} [A-Ja-j]{1}[0-9]{1}$";
        private const string ERROR_MSG = "Ships must be two coordinates separated by a space, e.g. 'A0 A4'";

        [Required]
        [RegularExpression(EXPRESSION, ErrorMessage = ERROR_MSG)]
        [ShipCoordsValid]
        [ShipValidInFleet]
        public string ShipOne { get; set; }
        
        [Required]
        [RegularExpression(EXPRESSION, ErrorMessage = ERROR_MSG)]
        [ShipCoordsValid]
        [ShipValidInFleet]
        public string ShipTwo { get; set; }
        
        [Required]
        [RegularExpression(EXPRESSION, ErrorMessage = ERROR_MSG)]
        [ShipCoordsValid]
        [ShipValidInFleet]
        public string ShipThree { get; set; }
        
        [Required]
        [RegularExpression(EXPRESSION, ErrorMessage = ERROR_MSG)]
        [ShipCoordsValid]
        [ShipValidInFleet]
        public string ShipFour { get; set; }
        
        [Required]
        [RegularExpression(EXPRESSION, ErrorMessage = ERROR_MSG)]
        [ShipCoordsValid]
        [ShipValidInFleet]
        public string ShipFive { get; set; }

        public string GetShip(int num)
        {
            switch (num)
            {
                case 1: return ShipOne;
                case 2: return ShipTwo;
                case 3: return ShipThree;
                case 4: return ShipFour;
                case 5: return ShipFive;
                default:
                    throw new ArgumentOutOfRangeException(nameof(num), "Should be 1-5");
            }
        }
        
        public string GetShipName(int num)
        {
            switch (num)
            {
                case 1: return nameof(ShipOne);
                case 2: return nameof(ShipTwo);
                case 3: return nameof(ShipThree);
                case 4: return nameof(ShipFour);
                case 5: return nameof(ShipFive);
                default:
                    throw new ArgumentOutOfRangeException(nameof(num), "Should be 1-5");
            }
        }

        public int GetShipNumber(string shipPropertyName)
        {
            switch (shipPropertyName)
            {
                case nameof(ShipOne): return 1;
                case nameof(ShipTwo): return 2;
                case nameof(ShipThree): return 3;
                case nameof(ShipFour): return 4;
                case nameof(ShipFive): return 5;
                default:
                    throw new ArgumentOutOfRangeException(nameof(shipPropertyName), $"'{shipPropertyName}' is not a valid property name of {nameof(CellState)}");
            }
        }

        public IEnumerable<(string propertyName, string shipCoords)> GetShips()
        {
            for (int i = 1; i <= 5; i++)
                yield return (GetShipName(i), GetShip(i));
        }
                
        public IEnumerable<(string propertyName, string[] shipCoords)> GetValidShipCoordPairs()
        {
            return GetShips()
                .Where(sc => sc.shipCoords != null)
                .Select(sc => (sc.propertyName, sc.shipCoords.Split(' ')))
                .Where(sct => sct.Item2.Length == 2);
        }

        public IEnumerable<(string propertyName, Ship ship)> GetValidStructuralShips()
        {
            foreach ((var propertyName, var coordPair) in GetValidShipCoordPairs())
            {
                Ship ship;
                try
                {
                    ship = new Ship(coordPair[0], coordPair[1]);
                }
                catch (ArgumentException)
                {
                    continue;
                }

                yield return (propertyName, ship);
            }
        }
    }

    /* THIS IS ALL A BIT HACKY AND NEEDS REWORK */

    // Valid length and not diagonal
    public class ShipCoordsValidAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var coords = ((string)value)?.Split(' ');

            if (coords?.Length != 2)
                return null; // Regex validation will get this

            Ship ship;
            try
            {
                ship = new Ship(coords[0], coords[1]);
            }
            catch (ArgumentException ex)
            {
                return new ValidationResult(ex.Message, new[] { validationContext.MemberName });
            }

            return null;
        }
    }

    // ship not overlapping or too many of the same size
    public class ShipValidInFleetAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var setupShips = (SetupShips)validationContext.ObjectInstance;
            var setupBoard = new SetupBoard();

            Ship thisShip = null;
            foreach (var ship in setupShips.GetValidStructuralShips())
            {
                if (ship.propertyName == validationContext.MemberName)
                {
                    thisShip = ship.ship;
                    continue;
                }

                // Add all other ships
                setupBoard.AddShip(ship.ship);
            }

            if (thisShip == null) // Isn't valid enough to test - TODO: Rework this to avoid the foreach, just exit
                return null; 

            // Now add target ship, to get error for this one specifically
            var result = setupBoard.AddShip(thisShip);

            if (!result.Success)
                return new ValidationResult(result.Error, new[] { validationContext.MemberName });            

            return null;
        }
    }    
}
