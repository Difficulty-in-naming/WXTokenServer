using OpenQA.Selenium;
using OpenQA.Selenium.Edge;
using System.Text;
using Newtonsoft.Json;
using System.Net;
using Cookie = OpenQA.Selenium.Cookie;

namespace GetWeixinToken;

class TokenServer
{
    private const string Url =
        "https://doc.weixin.qq.com/sheet/e3_AVYAiwbxAP4A44ZLWQpR5uWMh0hiC?scode=APIAPAc-AAkYhqbovy&tab=vfei91";
    private const string JsonPath = "Secret.txt";
    private static Dictionary<string,Cookie>? Cookies = null;
    private const string Address = "*";
    static int Main()
    {
        var settings = new JsonSerializerSettings();
        settings.Converters.Add(new CookieConverter());
        try
        {
            Cookies = JsonConvert.DeserializeObject<Dictionary<string, Cookie>?>(File.ReadAllText(JsonPath), settings);
        }
        catch(Exception ex)
        {
            Console.WriteLine(ex);
        }
        Console.OutputEncoding = Encoding.UTF8;
        EdgeOptions options = new EdgeOptions();
        options.AddArgument("--headless");
        //options.AddUserProfilePreference("download.default_directory", @"C:\Downloads");

        EdgeDriver driver = new EdgeDriver(options);
        HttpListener listener = new HttpListener();

        try
        {
            string url = $"http://{Address}:8080/";

            listener.Prefixes.Add(url);

            listener.Start();
            Console.WriteLine("Listening for requests on {0}", url);

        }
        catch (HttpListenerException e)
        {
            throw new Exception($"请先在cmd中运行: netsh http add urlacl url=http://*:你的address中的端口/ user=Everyone, address: {Address}", e);
        }


        // 处理请求
        while (true)
        {
            HttpListenerContext context = listener.GetContext();
            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;

            // 构造响应
            string responseString = GetSecret(driver);
            byte[] buffer = Encoding.UTF8.GetBytes(responseString);
            response.ContentLength64 = buffer.Length;
            response.OutputStream.Write(buffer, 0, buffer.Length);

            // 发送响应
            response.OutputStream.Flush();
            response.OutputStream.Close();
        }
        /*Console.WriteLine("=============================");
    
        while (true)
        {
            try
            {
                var s = driver.FindElement(By.XPath("/html/body/div[3]/div/div[1]/div[11]/div/div"));
                s.Click();
                Thread.Sleep(1000);
                var s1 = driver.FindElement(By.XPath("/html/body/div[12]/div/ul/div[11]/div/div/div/div/div/li"));
                s1.Click();
                Thread.Sleep(1000);
                var s2 = driver.FindElement(By.XPath("/html/body/div[9]/ul/div[1]/li"));
                s2.Click();
                break;
            }
            catch
            {
                Thread.Sleep(1000);
            }
        }*/
    }

    static string GetSecret(EdgeDriver driver)
    {
        try
        {
            
            if (Cookies != null && Cookies.TryGetValue("wedoc_sid",out var cookie))
            {
                if (cookie.Expiry > DateTime.Now)
                {
                    return JsonConvert.SerializeObject(Cookies);
                }
            }
            driver.Navigate().GoToUrl(Url);
            int time = 0;
            int flushTimes = 0;
            while (true)
            {
                if (time > 5000)
                {
                    driver.Navigate().Refresh();
                    flushTimes++;
                    time = 0;
                }

                if (flushTimes > 3)
                {
                    return "";
                }
                try
                {
                    driver.SwitchTo().Frame(0);
                    driver.SwitchTo().Frame(0);
                    var s = driver.FindElement(By.XPath("/html/body/div/div/div/div/div[2]/a"));
                    s.Click();
                    break;
                }
                catch
                {
                    Thread.Sleep(1000);
                    driver.SwitchTo().DefaultContent();
                    time += 1000;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
        Thread.Sleep(5000);

        foreach (var node in driver.Manage().Cookies.AllCookies)
        {
            Console.WriteLine(
                $"driver.Manage().Cookies.AddCookie(new Cookie(\"{node.Name}\",\"{node.Value}\",\"{node.Domain}\",\"{node.Path}\",\"{node.Expiry}\",\"{node.Secure}\",\"{node.IsHttpOnly}\",\"{node.SameSite}\");");
        }

        var dictionary = new Dictionary<string, Cookie>();
        foreach (var node in driver.Manage().Cookies.AllCookies)
        {
            if (!dictionary.ContainsKey(node.Name)) 
                dictionary.Add(node.Name, node);
        }

        Cookies = dictionary;
        var cache = JsonConvert.SerializeObject(Cookies);
        File.WriteAllText(JsonPath,cache);
        return cache;
    }
}