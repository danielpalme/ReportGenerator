
namespace Test
{
    public partial class PartialClassWithAutoProperties
    {
        public string Property2 { get; set; }

        public string ExpressionBodiedProperty => "Test";
    }
}
