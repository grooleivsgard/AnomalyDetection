using Castle.Core.Configuration;
using IntrusionDetectionSystem.DAL;
using Microsoft.Extensions.Logging;
using Models;

namespace IntrusionDetectionSystem;

public class Statistics
{
    //variance = (1/n - 1) sum(i= 1, -->n)(xi - avg(x))
    //st.dev = sq.root(variance)
    
    public static bool isDeviating(List<double> Values, long CurrentValues)
    {
        string description = null;
        bool isDeviating = false;

        double AvgHourly = Values[0];
        double AvgDaily = Values[1];
        double AvgWeekly = Values[2];

        double currVal = Convert.ToDouble(CurrentValues);

        if (Math.Abs(currVal - AvgHourly) > 10)
        {
            isDeviating = true;
            description = "Current value deviates from hourly average";
        }
        
        if (Math.Abs(currVal - AvgDaily) > 10)
        {
            isDeviating = true;
            description = "Current value deviates from daily average";
        }
        
        if (Math.Abs(currVal - AvgWeekly) > 10)
        {
            isDeviating = true;
            description = "Current value deviates from weekly average";
        }

        if (description != null)
        {
            Console.Write(description);
        }
        
        return isDeviating;
 
    }
    
    /**
         * Method computes the Standard Deviation based on a List of values
         */

        public static double ComputeStandardDeviation(List<long> values)
        {
            // Compute variance
            double total = 0;
            int n = 0;
            foreach (var i in values)
            {
                double delta = Math.Pow(Convert.ToDouble(i) - n, 2);
                total += delta;
                n ++;
            }
            double variance = total / n;
            double standardDev = Math.Sqrt(variance);

            return standardDev;
        }
    
}