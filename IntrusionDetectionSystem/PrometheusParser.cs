namespace IntrusionDetectionSystem
{
    public static class PrometheusParser 
    {
        public static  void  Parse (string payload) 
        {
           // Metrics metrics = new Metrics(); 
             var lines =  payload.Split('\n')
                .Select(_ => _.Trim())
               // .Where(_ => (!_.StartsWith("#")) && (!_.StartsWith("go")) ); 
               .Where(_ => _.StartsWith("hosts_src_dst"));
            
            foreach (string str in lines)
            
            Console.WriteLine(str); 
            
        }
    }
}