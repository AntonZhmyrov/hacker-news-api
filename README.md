# Hacker News API

## How to run the application 

1. Download the code from repository.
2. As the task description didn't specify in which way I should ship the app - please, just open the code in your favorite IDE - Visual Studio or JetBrains Rider.
3. Hit **Ctrl+F5** or **Run** the application.
4. For development purposes, I used the self-signed SSL certificate, which is offered to you by IDE. So, before running the app the first time, IDE will prompt you to install and trust one, please just click **Yes**.
5. The Swagger UI should open in your default browser where you can "play" with the API.

***Troubleshooting:*** In case the Browser shows that the connection is not secure, after running the application, please - reopen the browser, so that it pulls the freshly created certificate.

## Assumptions about the app and implementation details

Basically, the implemented service plays a role of API Gateway. It requests certain information from a third party service, simplifies it and outputs in a client-friendly manner. 
It consists of one endpoint which accepts one parameter - number of best stories to return the information for. In my opinion, there are 2 main bottlenecks in the current implemenation of the third party Hacker News API:
1. There is no rate limiting. We can easily flood the service with requests and make it go down.
2. In order to retrieve the details of best stories, we need to separately load each story from the third party. The amount of requests in this case equals to ***numberOfStories*** API parameter. We don't limit its value, so potentially, it could be thousands subrequests in only one call to our API.

I fixed those 2 bottlneckes by providing:
* fixed window rate limiting using the *Microsoft.AspNetCore.RateLimiting* nuget package. Now, our service will "protect" the Hacker News API from being overloaded by requests.
* the batch loading of details information for best stories. When we load the details for each of the best stories, the loading process is done in batches equal to 50 requests (I hardcoded it for development and demonstartion purposes) and we await each batch asynchronously using *await Task.WhenAll()*. It assures that even with one request, we will not produce thosuands of calls to get the details from Hacker News API third party.

The flow of getting the info for the best story is the following (let's assume that the ***numberOfStories*** equals to 2):
1. We fetch the best stories ids from Third Party Hacker News API and take the first 2.
2. And then for each of the ids received, we query the details data.
3. So, for 2 best stories there will be 3 requests to third party Hacker News API.
4. Then there is processing and mapping of the result and it is returned to a client.



