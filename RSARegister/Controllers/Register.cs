using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RSARegister.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]
namespace RSARegister.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Register : ControllerBase
    {
        private IHttpContextAccessor _accessor;
        IDynamoDBContext dBContext { get; set; }
        private readonly IAmazonDynamoDB _amazonDynamoDB;
        private bool _IsInited = false;
        public Register(IHttpContextAccessor accessor, IAmazonDynamoDB amazonDynamoDB)
        {
            _accessor = accessor;
            _amazonDynamoDB= amazonDynamoDB;
            AWSConfigsDynamoDB.Context.TypeMappings[typeof(ClientModel)]= new Amazon.Util.TypeMapping(typeof(ClientModel), nameof(ClientModel));
            var client = (AmazonDynamoDBClient)_amazonDynamoDB;
            dBContext = new DynamoDBContext(client);
        }
        private async Task Init()
        {
            var request = new ListTablesRequest
            {
                Limit = 10
            };
            var response = await _amazonDynamoDB.ListTablesAsync(request);
            var results = response.TableNames;
            if (!results.Contains(nameof(ClientModel)))
            {
                var createRequest = new CreateTableRequest
                {
                    TableName = nameof(ClientModel),
                    AttributeDefinitions = new List<AttributeDefinition>
                    {
                        new AttributeDefinition
                        {
                            AttributeName = nameof(ClientModel.ID),
                            AttributeType =ScalarAttributeType.S
                        }
                    },
                    KeySchema = new List<KeySchemaElement>
                    {
                        new KeySchemaElement
                        {
                            AttributeName = nameof(ClientModel.ID),
                            KeyType =KeyType.HASH //Partition key,
                        }
                    },
                    ProvisionedThroughput = new ProvisionedThroughput
                    {
                        ReadCapacityUnits = 2,
                        WriteCapacityUnits = 2
                    }
                };
                await _amazonDynamoDB.CreateTableAsync(createRequest);
                _IsInited=true;
            }
        }
        [HttpGet("RegisterClient")]
        public async Task<ResponseClientKey> RegisterClient()
        {
            try
            {
                if (!_IsInited)
                {
                    await Init();
                }
                RSAProvider rsa = new RSAProvider();
                ClientModel clientModel = new ClientModel()
                {
                    ID  = Guid.NewGuid().ToString(),
                    IP =_accessor?.HttpContext?.Connection?.RemoteIpAddress?.MapToIPv4()?.ToString(),
                    Hour=System.DateTime.Now.ToString("hh:mm:ss tt"),
                    PublicKey = rsa.CreatePublicKey(),
                    PrivateKey = rsa.CreatePrivateKey()
                };
                await dBContext.SaveAsync(clientModel);
                var response = new APIGatewayProxyResponse
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Body = clientModel.ID.ToString(),
                    Headers = new Dictionary<string, string> { { "Content-Type", "text/plain" } }
                };
                if (response.StatusCode != (int)HttpStatusCode.OK)
                {
                    return new ResponseClientKey(Guid.Empty, "Fatal error");
                }
                ResponseClientKey clientKey = new ResponseClientKey()
                {
                    Id = clientModel.ID,
                    Key = clientModel.PublicKey
                };

                return clientKey;
            }
            catch (Exception e)
            {
                return new ResponseClientKey(Guid.Empty,e.ToString());
            }
           
        }
        [HttpGet("SendKeys/{user}")]
        public async Task<ResponseClientKey> SendKeys(string user)
        {
            
            try
            {
                if (!_IsInited)
                {
                    await Init();
                }
                if (string.IsNullOrEmpty(user))
                {
                    return new ResponseClientKey(Guid.Empty, "No se pueden hacer busquedas con un Usuario vacio");
                }
                ClientModel client = await dBContext.LoadAsync<ClientModel>(user);
                if (client is null)
                {
                    return new ResponseClientKey(Guid.Empty,"Not Found");
                }
                ResponseClientKey clientKey = new()
                {
                    Id = user,
                    Key = client.PrivateKey
                };
                return clientKey;
            }
            catch (Exception e)
            {
                return new ResponseClientKey(Guid.Empty,e.ToString());
            }
            
        }

    }
}
