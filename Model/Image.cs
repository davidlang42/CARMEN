using System.ComponentModel.DataAnnotations;

namespace Model
{
    /// <summary>
    /// A stored image containing binary data.
    /// </summary>
    public class Image
    {
        [Key]
        public int ImageId { get; private set; }
        public string Name { get; set; } = "";
        public byte[] ImageData { get; set; } = new byte[0];
    }
}
