using System.Net.Sockets;
using System.Runtime.InteropServices.ComTypes;
using Castle.Core.Configuration;
using IntrusionDetectionSystem.DAL;
using Microsoft.Extensions.Logging;
using Models;

namespace IntrusionDetectionSystem;

public class Statistics
{

    public static List<double> calcData(List<long> dbLastHour, List<long> dbLastDay, List<long> dbLastWeek)
    { 
        
       
        
        List<double> hourlyData = computeStats(dbLastHour);
       List<double> dailyData = computeStats(dbLastDay);
       List<double> weeklyData = computeStats(dbLastWeek);

       //Merge three lists
       List<double> statsSummary = hourlyData.Concat(dailyData).Concat(weeklyData).ToList();
       
       return statsSummary;
    }
    
    public static List<double> computeStats(List<long> values)
    {
        // Compute variance
        double total = 0;
        int n = 0;
        double sum = 0;
        foreach (var i in values)
        {
            double delta = Math.Pow(Convert.ToDouble(i) - n, 2);
            total += delta;
            sum += i;
            n ++;
        }

        double mean = sum / n;
        double variance = total / n;
        double standardDev = Math.Sqrt(variance);

        List<double> statistics = new List<double>();
        statistics.Add(mean);
        statistics.Add(standardDev);

        return statistics;
    }




    public static bool compareValues(List<double> values, long currValue)
    {
        string description = "";
        bool isDeviating = false;
        

        double hourlyZScore = computeZscore(values[0], values[1], currValue);
        double dailyZScore = computeZscore(values[2], values[3], currValue);
        double weeklyZScore = computeZscore(values[4], values[5], currValue);

        if (hourlyZScore > 2 || hourlyZScore < 2)
        {
            isDeviating = true;
            description = "Current value deviates from hourly average";
        }
        
        if (dailyZScore > 2 || hourlyZScore < 2)
        {
            isDeviating = true;
            description = "Current value deviates from daily average";
        }

        if (weeklyZScore > 2 || hourlyZScore < 2)
        {
            isDeviating = true;
            description = "Current value deviates from weekly average";
        }
        
        Console.Write(description);
        return isDeviating;
 
    }
    
    /**
         * Method computes the Standard Deviation based on a List of values
        *  coming from the DB
         */

        

        public static double computeZscore(double standardDev,  double mean, long currValue)
        {
            double value = Convert.ToDouble(currValue);
            double zScore = (value - mean) / standardDev;

            return zScore;
        }
    
}