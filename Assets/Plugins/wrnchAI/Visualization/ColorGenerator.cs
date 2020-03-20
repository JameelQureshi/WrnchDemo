using System.Collections.Generic;
using UnityEngine;

namespace wrnchAI.Visualization
{
    /// <summary>
    /// Handle a collection of unique colors for character color assignation
    /// </summary>
    public class ColorCollection
    {
        private static int DefaultNumberOfColors = 10;
        private Stack<Color> m_availableColors;
        private List<Color> m_allColors;

        private Color GenerateNewColor()
        {
            Color newColor;
            do
            {
                //Generate a color in HSV space with 
                // H: [0.0; 1.0]
                // S: [0.0; 0.7]
                // V: 1.0
                newColor = UnityEngine.Random.ColorHSV(0f, 1f, 0.7f, 1f, 1f, 1f);
            } while (m_allColors.Contains(newColor));

            return newColor;
        }

        public ColorCollection()
        {
            m_availableColors = new Stack<Color>();
            m_allColors = new List<Color>();

            int generatedColors = 0;

            while (generatedColors < DefaultNumberOfColors)
            {
                var c = GenerateNewColor();
                m_availableColors.Push(c);
                m_allColors.Add(c);
                ++generatedColors;
            }
        }

        /// <summary>
        /// Get the next available color, or create a new one
        /// </summary>
        /// <returns> an unused color </returns>
        public Color GetNextColor()
        {
            if (m_availableColors.Count != 0)
            {
                return m_availableColors.Pop();
            }
            else
            {
                var c = GenerateNewColor();
                m_allColors.Add(c);
                return c;
            }
        }

        /// <summary>
        /// Reset all colors to originals
        /// </summary>
        public void Reset()
        {
            m_availableColors.Clear();
            foreach (var c in m_allColors)
                m_availableColors.Push(c);
        }

        /// <summary>
        /// Dispose a color and make it available again
        /// </summary>
        /// <param name="c"> color to dispose </param>
        public void DisposeColor(Color c)
        {
            m_availableColors.Push(c);
        }
    }
}