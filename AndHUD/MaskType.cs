namespace AndroidHUD
{
    /// <summary>
    /// Mask type to determine how to dim the background.
    /// </summary>
    public enum MaskType
    {
        /// <summary>
        /// No mask type
        /// </summary>
        None = 1,

        /// <summary>
        /// Show on top of clear background
        /// </summary>
        Clear = 2,

        /// <summary>
        /// Show on top of black dimmed background
        /// </summary>
        Black = 3
    }
}
