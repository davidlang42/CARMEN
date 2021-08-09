using System.ComponentModel.DataAnnotations;

namespace Carmen.ShowModel
{
    /// <summary>
    /// A stored image containing binary data.
    /// </summary>
    public class Image : INameOrdered
    {
        [Key]
        public int ImageId { get; private set; }
        public string Name { get; set; } = "";
        public byte[] ImageData { get; set; } = new byte[0];
    }
}
