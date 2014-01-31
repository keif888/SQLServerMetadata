using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;


namespace Microsoft.Msagl.Drawing {
    /// <summary>
    /// a generic class for representing a couple of entities
    /// </summary>
    /// <typeparam name="TFirst"></typeparam>
    /// <typeparam name="TSecond"></typeparam>
    public class Couple<TFirst, TSecond> {
        TFirst first;
        TSecond second;
        /// <summary>
        /// the first element
        /// </summary>
        public TFirst First {
            get { return first; }
            set { first = value; }
        }
        /// <summary>
        /// the second element
        /// </summary>
        public TSecond Second {
            get { return second; }
            set { second = value; }
        }

        /// <summary>
        /// create a couple
        /// </summary>
        /// <param name="firstElement"></param>
        /// <param name="secondElement"></param>
        public Couple(TFirst firstElement, TSecond secondElement) {
            first = firstElement;
            second = secondElement;
        }
/// <summary>
/// 
/// </summary>
/// <returns></returns>
        public override int GetHashCode() {
            return first.GetHashCode() | second.GetHashCode();
        }
/// <summary>
/// overrides the equality
/// </summary>
/// <param name="obj"></param>
/// <returns></returns>
        public override bool Equals(object obj) {
            Couple<TFirst, TSecond> c = obj as Couple<TFirst, TSecond>;
            if (c == null)
                return false;

            return c.first.Equals(first) && c.second.Equals(second);

        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString() {
            return String.Format(CultureInfo.InvariantCulture, "({0},{1}", first, second);
        }

    }
}
