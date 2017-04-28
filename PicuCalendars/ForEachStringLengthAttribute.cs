using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PicuCalendars
{

    /// <summary>
    /// Validation attribute to assert each member of an IEnumerable<string> property, field or parameter does not exceed a maximum length
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    public sealed class EachStringLengthAttribute : StringLengthAttribute
    {
        /// <summary>
        /// Gets the maximum acceptable length of the string
        /// </summary>
        public new int MaximumLength
        {
            get { return base.MaximumLength; }
        }

        /// <summary>
        /// Gets or sets the minimum acceptable length of the string
        /// </summary>
        public new int MinimumLength
        {
            get { return base.MinimumLength; }
            set { base.MinimumLength = value; }
        }

        /// <summary>
        /// Constructor that accepts the maximum length of the string.
        /// </summary>
        /// <param name="maximumLength">The maximum length, inclusive.  It may not be negative.</param>
        public EachStringLengthAttribute(int maximumLength)
            : base(maximumLength)
        {
        }

        /// <summary>
        /// Override of <see cref="ValidationAttribute.IsValid(object)"/>
        /// </summary>
        /// <remarks>This method returns <c>true</c> if the <paramref name="value"/> is null.  
        /// It is assumed the <see cref="RequiredAttribute"/> is used if the value may not be null.</remarks>
        /// <param name="value">The value to test.</param>
        /// <returns><c>true</c> if the value is null or less than or equal to the set maximum length</returns>
        /// <exception cref="InvalidOperationException"> is thrown if the current attribute is ill-formed.</exception>

        public override bool IsValid(object value)
        {
            return ((IEnumerable<string>)value).All(s => base.IsValid(s));
        }

    }
}
