using NetCorePal.Extensions.AspNetCore;
using System.Net;
using System.Net.Http.Json;

namespace NetCorePal.Web.UnitTests
{
    public class ProgramTests(MyWebApplicationFactory factory) : IClassFixture<MyWebApplicationFactory>
    {

        [Fact]
        public void HealthCheckTest()
        {
            var client = factory.CreateClient();
            var response = client.GetAsync("/health").Result;
            Assert.True(response.IsSuccessStatusCode);
        }


        [Fact]
        public async Task SagaTest()
        {
            var client = factory.CreateClient();
            await Task.Delay(2000);
            var response = client.GetAsync("/saga").Result;
            Assert.True(response.IsSuccessStatusCode);
        }



        [Fact]
        public async Task KnownExceptionTest()
        {
            var client = factory.CreateClient();
            await Task.Delay(2000);
            var response = client.GetAsync("/knownexception").Result;
            Assert.True(response.IsSuccessStatusCode);
            var data = await response.Content.ReadFromJsonAsync<ResponseData>();
            Assert.NotNull(data);
            Assert.Equal("test known exception message", data.Message);
            Assert.Equal(33, data.Code);
            Assert.False(data.Success);
        }


        [Fact]
        public async Task UnknownExceptionTest()
        {
            var client = factory.CreateClient();
            await Task.Delay(2000);
            var response = client.GetAsync("/unknownexception").Result;
            Assert.True(!response.IsSuccessStatusCode);
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
            var data = await response.Content.ReadFromJsonAsync<ResponseData>();
            Assert.NotNull(data);
            Assert.Equal("未知错误", data.Message);
            Assert.Equal(99999, data.Code);
            Assert.False(data.Success);
        }
    }
}