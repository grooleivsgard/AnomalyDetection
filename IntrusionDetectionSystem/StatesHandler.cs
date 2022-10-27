using Microsoft.AspNetCore.Http.Features;

namespace IntrusionDetectionSystem;

public class StatesHandler
{
    /*
        public static void Main(string[] args)
        {
            int from_state = 0;
            int to_state = 1;

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
*/
        public static bool HandleState(int from_state, int to_state)
        {
            bool[,] states = new bool[4, 4]
            {
                { false, true, false, false },
                { false, false, true, false },
                { true, false, false, true },
                { true, false, false, false }
            };
            
            if ((from_state > 3 || from_state < 0) || (to_state < 0 || to_state > 3))
            {
                throw new ArgumentOutOfRangeException("States not allowed!");
            }
            
            return states[from_state, to_state];

            /*     
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
            */
        }
       
    
}

