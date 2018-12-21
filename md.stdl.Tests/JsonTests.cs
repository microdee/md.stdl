using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;
using md.stdl.json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace md.stdl.Tests
{
    public class JsonTests
    {
        private const string JsonText = @"
{
    ""yolo"": ""swag"",
    ""data"": {
        ""foo"" : ""wizz"",
        ""bar"" : [
            {
                ""state"" : ""swaggery"",
                ""boink"" : true
            },
            {
                ""state"" : ""jazz"",
                ""boink"" : false
            }
        ],
        ""state"" : {
            ""restCount"" : 3,
            ""coeff"" : 1.29348756
        }
    }
}";

        private const string YamlText = @"
template: &template
  yolo: fizz
  music: lalala
  food:
    ingredient: something
  data:
    foo: buzz
    jazz: derp
    pop: bump
content:
  <<: *template
  yolo: swag
  data:
    foo: wizz
    bar:
      - state : swaggery
        boink : true
      - state : jazz,
        boink : false
    state:
      restCount : 3,
      coeff : 1.29348756"
;

        [Fact]
        public void JsonTest()
        {
            var json = JToken.Parse(JsonText);
            var state = json.GetFromPath("data.bar[0].state", "");
            Assert.Equal("swaggery", state);
        }

        [Fact]
        public void YamlTest()
        {
            var json = YamlToJson.ParseYamlToJson(YamlText);
            Assert.False(((JObject)json["content"]).ContainsKey("<<"));
        }
    }
}
