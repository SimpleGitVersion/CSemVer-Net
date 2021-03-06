using System;

namespace CSemVer
{
    /// <summary>
    /// Defines a "Min-Max" (this is the string representation) filter of <see cref="PackageQuality"/>.
    /// By default, this filter accepts everything (<see cref="PackageQuality.None"/> is the same as <see cref="PackageQuality.CI"/> for <see cref="Min"/>
    /// and the same as <see cref="PackageQuality.Release"/> for <see cref="Max"/>).
    /// </summary>
    public readonly struct PackageQualityFilter : IEquatable<PackageQualityFilter>
    {
        /// <summary>
        /// Gets the minimal package quality. <see cref="PackageQuality.None"/> is the same as <see cref="PackageQuality.CI"/>.
        /// </summary>
        public PackageQuality Min { get; }


        /// <summary>
        /// Gets the maximal package quality. <see cref="PackageQuality.None"/> is the same as <see cref="PackageQuality.Release"/>.
        /// </summary>
        public PackageQuality Max { get; }

        /// <summary>
        /// Gets whether <see cref="Min"/> is relevant (not <see cref="PackageQuality.None"/> nor <see cref="PackageQuality.CI"/>).
        /// </summary>
        public bool HasMin => Min != PackageQuality.None && Min != PackageQuality.CI;

        /// <summary>
        /// Gets whether <see cref="Max"/> is relevant (not <see cref="PackageQuality.None"/> nor <see cref="PackageQuality.Release"/>).
        /// </summary>
        public bool HasMax => Max != PackageQuality.None && Max != PackageQuality.Release;

        /// <summary>
        /// Gets whether this filter allows the specified quality.
        /// </summary>
        /// <param name="q">The quality to challenge. <see cref="PackageQuality.None"/> is never accepted.</param>
        /// <returns>Whether <paramref name="q"/> is accepted or not.</returns>
        public bool Accepts( PackageQuality q ) => q != PackageQuality.None
                                                    && (!HasMin || q >= Min)
                                                    && (!HasMax || q <= Max);

        /// <summary>
        /// Initializes a new filter (min and max are reordered if needed).
        /// </summary>
        /// <param name="min">The minimal quality.</param>
        /// <param name="max">The maximal quality.</param>
        public PackageQualityFilter( PackageQuality min, PackageQuality max )
        {
            if( min > max )
            {
                Min = max;
                Max = min;
            }
            else
            {
                Min = min;
                Max = max;
            }
        }

        /// <summary>
        /// Initializes a new filter by parsing a nullable string.
        /// Throws an <see cref="ArgumentException"/> on invalid syntax: use <see cref="TryParse(string, out PackageQualityFilter)"/>
        /// to handle invalid syntax.
        /// </summary>
        /// <param name="s">The string. Can be null or empty.</param>
        public PackageQualityFilter( string s )
        {
            if( !String.IsNullOrWhiteSpace( s ) )
            {
                if( !TryParse( s, out PackageQualityFilter p ) ) throw new ArgumentException( "Invalid PackageQualityFilter syntax." );
                Min = p.Min;
                Max = p.Max;
            }
            else Min = Max = PackageQuality.None;
        }

        /// <summary>
        /// Overridden to return "<see cref="Min"/>-<see cref="Max"/>".
        /// </summary>
        /// <returns>The "Min-Max" string.</returns>
        public override string ToString() => Min.ToString() + '-' + Max.ToString();

        /// <summary>
        /// Attempts to parse a string as a <see cref="PackageQualityFilter"/>.
        /// Examples:
        /// "Release" (is the same as "Release-Release"): only <see cref="PackageQuality.Release"/> is accepted
        /// "CI-Release" (is the same as "-Release" or "CI-" or ""): everything is accepted.
        /// "-ReleaseCandidate" (same as "CI-ReleaseCandidate"): everything except Release.
        /// "Exploratory-Preview": No CI, ReleaseCandidate, nor Release.
        /// </summary>
        /// <param name="s">The string to parse. Can not be null but can be empty.</param>
        /// <param name="q">The result.</param>
        /// <returns>True on success, false on error.</returns>
        public static bool TryParse( string s, out PackageQualityFilter q )
        {
            var minMax = s.Replace( " ", "" )
                          .Split( '-' );
            if( minMax.Length == 1 )
            {
                var only = minMax[0];
                if( only.Length == 0 )
                {
                    q = new PackageQualityFilter();
                    return true;
                }
                if( TryParse( only, out PackageQuality both, PackageQuality.None ) )
                {
                    q = new PackageQualityFilter( both, both );
                    return true;
                }
            }
            else if( minMax.Length == 2
                && TryParse( minMax[0], out PackageQuality min, PackageQuality.CI )
                && TryParse( minMax[1], out PackageQuality max, PackageQuality.Release ) )
            {
                q = new PackageQualityFilter( min, max );
                return true;
            }
            q = new PackageQualityFilter();
            return false;
        }

        static bool TryParse( string s, out PackageQuality q, PackageQuality def )
        {
            q = def;
            if( s.Length == 0 ) return true;
            return Enum.TryParse( s, out q );
        }

        /// <summary>
        /// Implements equality operator.
        /// </summary>
        /// <param name="other">Other filter.</param>
        /// <returns>True on success, false if other is different than this one.</returns>
        public bool Equals( PackageQualityFilter other ) => ((!HasMin && !other.HasMin) || Min == other.Min)
                                                            &&
                                                            ((!HasMax && !other.HasMax) || Max == other.Max);

        /// <summary>
        /// Overridden to call <see cref="Equals(PackageQualityFilter)"/>.
        /// </summary>
        /// <param name="obj">The other object.</param>
        /// <returns>True on success, false if other is not a filter or is different than this one.</returns>
        public override bool Equals( object obj ) => obj is PackageQualityFilter f && Equals( f );

        /// <summary>
        /// Overriden to match <see cref="Equals(PackageQualityFilter)"/>.
        /// </summary>
        /// <returns>The hsh code.</returns>
        public override int GetHashCode() => (HasMin ? ((int)Min << 8) : 0) | (HasMax ? (int)Max : 0);
    }
}
