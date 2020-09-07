
namespace Test
{
    public class CodeContract_Target : CodeContract_Interface
    {
        public int Calculate(int value)
        {
            if (value < 0)
            {
                return 0;
            }
            else
            {
                return 1;
            }
        }
    }
}
