using System.Xml.Serialization;

namespace NavData.Services.Models;

public class CifpEdition
{
    [XmlRoot(ElementName = "status")]
    public class Status
    {
        [XmlAttribute(AttributeName = "code")] public int Code { get; set; }

        [XmlAttribute(AttributeName = "message")]
        public required string Message { get; set; }
    }

    [XmlRoot(ElementName = "product")]
    public class Product
    {
        [XmlAttribute(AttributeName = "productName")]
        public required string ProductName { get; set; }

        [XmlAttribute(AttributeName = "url")] public required string Url { get; set; }
    }

    [XmlRoot(ElementName = "edition")]
    public class Edition
    {
        [XmlElement(ElementName = "editionDate")]
        public required string EditionDate { get; set; }

        [XmlElement(ElementName = "editionNumber")]
        public int EditionNumber { get; set; }

        [XmlElement(ElementName = "product")] public required Product Product { get; set; }

        [XmlAttribute(AttributeName = "editionName")]
        public required string EditionName { get; set; }

        [XmlAttribute(AttributeName = "format")]
        public required string Format { get; set; }

        [XmlText] public required string Text { get; set; }
    }

    [XmlRoot(ElementName = "productSet", Namespace = "http://arpa.ait.faa.gov/arpa_response")]
    public class ProductSet
    {
        [XmlElement(ElementName = "status")] public required Status Status { get; set; }

        [XmlElement(ElementName = "edition")] public required Edition Edition { get; set; }

        [XmlText] public required string Text { get; set; }
    }
}
