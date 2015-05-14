/* Copyright (C) 2015 Kevin Boronka
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE
 * LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
 * CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
 * SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
 * INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
 * CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
 * POSSIBILITY OF SUCH DAMAGE.
 */

using System;
using System.ComponentModel;
using System.IO;
using System.Net.Sockets;
using System.Text;

using sar.Tools;

namespace sar.Http
{
	public enum HttpStatusCode
	{
		[Description("OK")]
		OK = 200,
		FOUND = 302,
		NOTFOUND = 404,
		SERVERERROR=500
	};

	public class HttpResponse
	{
		public const string PDF_RENDER = "-pdf-render";
		private TcpClient socket;
		private NetworkStream stream;
		private Encoding encoding;

		private HttpRequest request;
		private HttpContent content;
		
		private bool pdfRender {get; set;}
		
		public byte[] bytes;
		
		public byte[] Bytes
		{
			get { return this.bytes; }
		}
		
		public HttpResponse(HttpRequest request, TcpClient socket)
		{
			this.request = request;
			this.encoding = Encoding.ASCII;
			this.socket = socket;
			this.stream = this.socket.GetStream();
			
			try
			{
				if (this.request.Path.ToLower().EndsWith(PDF_RENDER, StringComparison.CurrentCulture))
				{
					this.request.Path = StringHelper.TrimEnd(this.request.Path, PDF_RENDER.Length);
					this.pdfRender = true;
				}
				
				if (this.request.Path == @"")
				{
					if (HttpController.Primary == null) throw new ApplicationException("Primary Controller Not Defined");
					if (HttpController.Primary.PrimaryAction == null) throw new ApplicationException("Primary Action Not Defined");
	
					this.content = HttpController.RequestPrimary(this.request);
				}
				else if (this.request.Path.ToLower().EndsWith(@"-pdf", StringComparison.CurrentCulture))
				{
					string url = "http://localhost:" + request.Server.Port.ToString() + this.request.FullUrl + PDF_RENDER;
					
					url = url.Replace(this.request.Path, StringHelper.TrimEnd(this.request.Path, 4));
					
					this.content = HttpContent.GetPDF(url);
				}
				else if (this.request.Path.ToLower() == @"info")
				{
					this.content = HttpController.RequestAction("Debug", "Info", this.request);
				}
				else if (HttpController.ActionExists(this.request))
				{
					this.content = HttpController.RequestAction(this.request);
				}
				else
				{
					this.content = HttpContent.Read(this.request.Server, this.request.Path);
				}
				
				if (this.content is HttpErrorContent)
				{
					this.bytes = this.ConstructResponse(HttpStatusCode.SERVERERROR);
				}
				else
				{
					this.bytes = this.ConstructResponse(HttpStatusCode.OK);
				}
			}
			catch (FileNotFoundException ex)
			{
				Program.Log(ex);
				this.content = ErrorController.Display(this.request, ex, HttpStatusCode.NOTFOUND);
				this.bytes = this.ConstructResponse(HttpStatusCode.SERVERERROR);			}
			catch (Exception ex)
			{
				Program.Log(ex);
				this.content = ErrorController.Display(this.request, ex, HttpStatusCode.SERVERERROR);
				this.bytes = this.ConstructResponse(HttpStatusCode.SERVERERROR);
			}
		}
		
		private byte[] ConstructResponse(HttpStatusCode status)
		{
			// Construct response header
			
			const string eol = "\r\n";

			// status line
			string responsePhrase = Enum.GetName(typeof(HttpStatusCode), status);
			string response = "HTTP/1.0" + " " + ((int)status).ToString() + " " + responsePhrase + eol;
			
			byte [] contentBytes = this.content.Render();
			// content details
			response += "Content-Type: " + this.content.ContentType + eol;
			response += "Content-Length: " + (contentBytes.Length).ToString() + eol;
			response += "Server: " + @"sar\" + AssemblyInfo.SarVersion + eol;

			response += "Access-Control-Allow-Origin: *" + eol;
			response += "Access-Control-Allow-Methods: POST, GET" + eol;
			response += "Access-Control-Max-Age: 1728000" + eol;
			response += "Access-Control-Allow-Credentials: true" + eol;
			
		
	
			if (this.pdfRender) response += "X-Content-Type-Options: " + "pdf-render" + eol;

			// other
			response += "Connection: close";
			// terminate header
			response += eol + eol;
			
			return StringHelper.CombineByteArrays(Encoding.ASCII.GetBytes(response), contentBytes);
		}
	}
}
