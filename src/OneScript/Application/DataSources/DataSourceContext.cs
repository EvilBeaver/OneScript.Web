using MongoDB.Bson;
using MongoDB.Driver;
using ScriptEngine.Machine.Contexts;
using ScriptEngine;
using ScriptEngine.HostedScript.Library;
using ScriptEngine.Machine;

namespace OneScript.WebHost.Application.DataSources
{
    [ContextClass("МенеджерВнешнихИсточниковДанных", "ExternalDataSourcesManager")]
    public class DataSourceContext : AutoContext<DataSourceContext>
    {
        private static RuntimeEnvironment _globalEnv;
        private static MongoClient client;
        private static IMongoDatabase currentDatabase;

        public DataSourceContext(RuntimeEnvironment env)
        {
            _globalEnv = env;
            client = new MongoClient();
            currentDatabase = null;
        }

        [ContextMethod("Подключится")]
        public void Connect(string connString, string dbPreffix)
        {
            client = new MongoClient(connString);
            currentDatabase = client.GetDatabase(dbPreffix);
        }

        [ContextMethod("Отключиться")]
        public void Disconnect()
        {
            currentDatabase = null;
            client = new MongoClient();
        }

        [ContextMethod("ДобавитьДокументВКоллекцию")]
        public string AddDocToCollection(string collection, IValue doc)
        {
            var docCollection = currentDatabase.GetCollection<BsonDocument>(collection);
            docCollection.InsertOne(doc.ToBsonDocument());
            
            return "здесь будет ключ документа кстати";
        }
        
        [ContextMethod("ПрочитатьВсюКоллекцию")]
        public ArrayImpl ReadFullCollection(string collection)
        {
            
            //todo тут лучше всего читать не полностью - но 1С-ники привыкли читать полностью в память
            // скорее всего нужно реализовать интерфейс для каждого
            return new ArrayImpl();
            
        }

        
        
        
    }
}