using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityInspector.PropertyAttributes
{
    public class DrawIfFalseAttribute : PropertyAttribute
    {
        private string variableName;
        public string VariableName
        {
            get
            {
                return this.variableName;
            }
        }

        public DrawIfFalseAttribute(string variableName)
        {
            this.variableName = variableName;
        }
    }
}
