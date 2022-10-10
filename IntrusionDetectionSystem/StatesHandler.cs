using Microsoft.AspNetCore.Http.Features;

namespace IntrusionDetectionSystem;

public class StatesHandler
{

    class Endpoint
    {

        public static void Main(string[] args)
        {
            int from_state = 2;
            int to_state = 0;

            bool isAllowed = HandleState(from_state, to_state);

            if (isAllowed)
            {
                Console.WriteLine("OK");
            }
            else
            {
                Console.WriteLine("Something fishy");
            }
        }

        public static bool HandleState(int from_state, int to_state)
        {
            bool[,] states = new bool[4, 4]
            {
                { false, true, false, false },
                { false, false, true, false },
                { true, false, false, true },
                { true, false, false, false }
            };

            for (int i = 0; i <= states.Length;)
            {
                if (i == from_state)
                {
                    for (int j = 0; j <= states.Length;)
                    {
                        if (j == to_state)
                        {
                            if (states[i, j])
                            {
                                return true;
                         
                            }

                            return false;
                        }

                        j++;

                    }
                }

                i++;
            }
            return false;
        }
        
    }
    
}

