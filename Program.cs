//dotnet-svcutil tesztjogviszony.wsdl
//dotnet new console
//dotnet add package System.ServiceModel
//dotnet add package System.ServiceModel.Duplex
//dotnet add package System.ServiceModel.Http
//dotnet add package System.ServiceModel.NetTcp
//dotnet add package System.ServiceModel.Security
//dotnet add package System.Xml.XmlSerializer
//dotnet add package System.Xml.ReaderWriter

using System;
using ServiceReference;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Xml;
using System.Xml.Serialization;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Description;

//https://bbrauns1.wordpress.com/2019/11/07/how-to-add-soap-ws-security-usernametoken-profile-to-net-core-3-0-client/

public class Password
{
	[XmlAttribute] public string Type { get; set; }

	[XmlText] public string Value { get; set; }
}

public class UsernameToken
{
	[XmlElement] public string Username { get; set; }

	[XmlElement] public Password Password { get; set; }
}

public class Security : MessageHeader
{
	public Security()
	{
		UsernameToken = new UsernameToken();
	}

	public UsernameToken UsernameToken { get; set; }

	public override string Name => GetType().Name;

	public override string Namespace => "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd";

	public override bool MustUnderstand => true;

	protected override void OnWriteHeaderContents(XmlDictionaryWriter writer, MessageVersion messageVersion)
	{
		var serializer = new XmlSerializer(typeof(UsernameToken));
		serializer.Serialize(writer, UsernameToken);
	}
}


public class WsSecurityMessageInspector : IClientMessageInspector  
{
  	private readonly string password;
	private readonly string username;

	public WsSecurityMessageInspector(string username, string password) : base()
	{
		this.username = username;
		this.password = password;
	}

    public object BeforeSendRequest(ref System.ServiceModel.Channels.Message request, IClientChannel channel)  
    {  
		var header = new Security
		{
			UsernameToken =
			{
				Password = new Password
				{
					Value = password,
					Type =	"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-username-token-profile-1.0#PasswordText"
				},
				Username = username
			}
		};
		request.Headers.Add(header);

        Console.WriteLine("BeforeSendRequest called:"); 
        Console.WriteLine($"{request}");

        return null;
    }  

    public void AfterReceiveReply(ref System.ServiceModel.Channels.Message reply, object correlationState)  
    {  
        Console.WriteLine("AfterReceiveReply called");  
        Console.WriteLine($"{reply}");
        reply.Headers.Clear();
    }  
}

// Endpoint behavior  
public class WsSecurityEndpointBehavior : IEndpointBehavior  
{

	private readonly string password;
	private readonly string username;

	public WsSecurityEndpointBehavior(string username, string password) : base()
	{
		this.username = username;
		this.password = password;
	}

    public void AddBindingParameters(ServiceEndpoint endpoint, System.ServiceModel.Channels.BindingParameterCollection bindingParameters)  
    {  
         // No implementation necessary  
    }  
  
    public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)  
    {  
        clientRuntime.ClientMessageInspectors.Add(new WsSecurityMessageInspector(username,password));
    }  
  
    public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)  
    {  
         // No implementation necessary  
    }  
  
    public void Validate(ServiceEndpoint endpoint)  
    {  
         // No implementation necessary  
    }  
}  
public partial class m_jogviszonyV10Client : jogviszonyV10Client {

    public async System.Threading.Tasks.Task<ServiceReference.jogviszonyV10Response> jogviszonyV10Async(ServiceReference.jogviszonyV10Request request)
    {
        return await Channel.jogviszonyV10Async(request);
    }

    public async System.Threading.Tasks.Task<ServiceReference.jogviszonyTAJV10Response> jogviszonyTAJV10Async(ServiceReference.jogviszonyTAJV10Request request)
    {
        return await Channel.jogviszonyTAJV10Async(request);
    }
}


namespace ojote
{
    class Program
    {
        static void Main(string[] args)
        {
            var l_jogviszonyV10Client = new m_jogviszonyV10Client();
            l_jogviszonyV10Client.Endpoint.EndpointBehaviors.Add(new WsSecurityEndpointBehavior("username","password"));

            string l_program_azon = "program_azon", l_ruser = "ruser", l_taj = "123456789";
            jogviszonyTAJV10Request l_request = new jogviszonyTAJV10Request( l_program_azon, l_ruser, l_taj);
            System.Threading.Tasks.Task<ServiceReference.jogviszonyTAJV10Response> l_jogviszonyTAJV10Response;

            l_jogviszonyTAJV10Response = l_jogviszonyV10Client.jogviszonyTAJV10Async(l_request);
            l_jogviszonyTAJV10Response.Wait();
        }

    }
}
