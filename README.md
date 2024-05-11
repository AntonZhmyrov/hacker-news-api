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
2. In order to retrieve the details of best stories, we need to separately load each story from the third party. The amount of requests in this case equals to ***numberOfStories*** API parameter plus one request to get best stories ids so --> ***numberOfRequests = numberOfStories + 1***. We don't limit the number of stories value, so potentially, it could be thousands subrequests in only one call to our API.

I fixed those 2 bottlneckes by providing:
* fixed window rate limiting using the *Microsoft.AspNetCore.RateLimiting* nuget package. Now, our service will "protect" the Hacker News API from being overloaded by requests.
* the batch loading of details information for best stories. When we load the details for each of the best stories, the loading process is done in batches equal to 50 requests (I hardcoded it for development and demonstartion purposes) and we await each batch asynchronously using *await Task.WhenAll()*. It assures that even with one request, we will not produce thosuands of calls to get the details from Hacker News API third party.

The flow of getting the info for the best story is the following (let's assume that the ***numberOfStories*** equals to 2):
1. We fetch the best stories ids from Third Party Hacker News API and take the first 2.
2. And then for each of the ids received, we query the details data.
3. So, for 2 best stories there will be 3 requests to third party Hacker News API.
4. Then there is processing and mapping of the result and it is returned to a client.

## Possible enhancements and improvements

1. The shipment of application could be done using Docker. We could package it in container and run using Docker. This was not done for simplicity purposes and irrelevance in this case. We have only one small service that communicates with third party which we don't control. If we controlled and owned the second service (Hacker News API) or maybe had a DB to communicate with - Docker would be a great tool for us here.
2. Introduce integration testing. We can enhance our testing using the WebApplicationFactory and WireMock (to mock third party Hacker Service News) and have integration tests to fully check the request pipeline, rate limiting, retries, endpoint outputs etc.
3. I would probably discuss the business purpose of our API to get best story details. If we want to output the Hacker News in some sort of Feed (like in Twitter or Instagram), then we need to introduce pagination. Requirements for the task didn't say anything about that explicitly, so a confirmation would be needed. But, if we need to display it somehow, or any other service will consume data from our API, pagination would help in limiting the requests and making the response size more limited and manageable. Right now, the pagination is probably not needed - as best stories API holds only 200 records. But if it changes, it will be one more reason to introduce pagination.


