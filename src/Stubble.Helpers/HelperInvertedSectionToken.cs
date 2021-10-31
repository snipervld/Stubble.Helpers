using System.Collections.Immutable;
using Stubble.Core.Imported;
using Stubble.Core.Tokens;

namespace Stubble.Helpers
{
    public class HelperInvertedSectionToken : BlockToken<HelperInvertedSectionToken>, INonSpace, IHelperCallInfo
    {
        /// <summary>
        /// Gets or sets the starting position of the tag
        /// </summary>
        public int StartPosition { get; set; }

        /// <summary>
        /// Gets or sets the end position of the tag
        /// </summary>
        public int EndPosition { get; set; }

        /// <summary>
        /// Gets or sets the sections name
        /// </summary>
        public string SectionName { get; set; }

        public ImmutableArray<HelperArgument> Args { get; set; }

        public sealed override string Identifier => SectionName;

        /// <summary>
        /// Gets or sets the string definition of the section content
        /// </summary>
        public StringSlice SectionContent { get; set; }

        /// <summary>
        /// Checks if one section tag is equal to another
        /// </summary>
        /// <param name="other">The other section tag</param>
        /// <returns>If the section tags are equal</returns>
        public override bool Equals(HelperInvertedSectionToken other)
        {
            if (other == null)
            {
                return false;
            }

            if (Children != null && other.Children != null)
            {
                if (Children.Count != other.Children.Count)
                {
                    return false;
                }

                for (var i = 0; i < Children.Count; i++)
                {
                    var equal = other.Children[i].Equals(Children[i]);

                    if (!equal)
                    {
                        return false;
                    }
                }
            }

            return
                !(Children == null & other.Children != null)
                    && !(Children != null & other.Children == null)
                    && other.IsClosed == IsClosed
                    && other.SectionName == SectionName
                    && other.StartPosition == StartPosition
                    && other.EndPosition == EndPosition
                    && CompareHelper.CompareImmutableArraysWithEquatable(Args, other.Args);
        }

        /// <summary>
        /// Checks if an object is equal to this section tag
        /// </summary>
        /// <param name="obj">The object to check</param>
        /// <returns>If the object is equal to this tag</returns>
        public override bool Equals(object obj)
        {
            return obj is HelperInvertedSectionToken a && Equals(a);
        }

        /// <summary>
        /// Gets the hash code for the tag
        /// </summary>
        /// <returns>The hashcode</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ StartPosition;
                hashCode = (hashCode * 397) ^ EndPosition;
                hashCode = (hashCode * 397) ^ (SectionName?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ Args.GetHashCode();

                return hashCode;
            }
        }
    }
}
