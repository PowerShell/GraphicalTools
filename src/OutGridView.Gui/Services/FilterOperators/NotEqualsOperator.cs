using System;
namespace OutGridView.Application.Services.FilterOperators
{
    public class NotEqualsOperator : IStringFilterOperator
    {
        public bool HasValue { get; } = true;
        public string Value { get; set; }
        public bool Execute(string input)
        {
            return !input.Equals(Value, StringComparison.CurrentCultureIgnoreCase);
        }
        public string GetPowerShellString()
        {
            return $"-NE \'{Value}\'";
        }
    }
}