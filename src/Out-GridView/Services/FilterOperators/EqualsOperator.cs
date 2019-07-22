using System;
using System.Collections.Generic;
using System.Management.Automation;

namespace OutGridView.Services.FilterOperators
{
    public class EqualsOperator : IFilterOperator
    {
        public bool HasValue { get; } = true;
        public string Value { get; set; }
        public bool Execute(string input)
        {
            return input.Equals(Value);
        }
    }
}