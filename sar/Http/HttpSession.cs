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
using System.Collections.Generic;

namespace sar.Http
{
	public class HttpSession
	{
		#region static
		
		private static Dictionary<string, HttpSession> sessions;
		private static string sessionLock = "";
		
		public static bool Contains(string id)
		{
			bool result;
			lock (sessionLock)
			{
				if (sessions == null) sessions = new Dictionary<string, HttpSession>();
				result = sessions.ContainsKey(id);
			}
			
			return result;
		}
		
		public static HttpSession Find(string id)
		{
			HttpSession responce;
			
			lock(sessionLock)
			{
				if (sessions == null) sessions = new Dictionary<string, HttpSession>();

				if (sessions.ContainsKey(id))
				{
					responce = sessions[id];
					responce.LastRequest = DateTime.Now;
				}
				else
				{
					responce = new HttpSession();
				}
			}
			
			
			return responce;
		}
		
		#endregion
		
		
		private string dataLock;
		public string ID { get; private set; }
		public DateTime CreationDate { get; private set; }
		public DateTime LastRequest { get; set; }
		public DateTime ExpiryDate { get { return LastRequest.AddDays(MAX_LIFE); } }

		public const int MAX_LIFE = 2;
		
		private Dictionary<string, object> data;
		public Dictionary<string, object> Data
		{
			get
			{
				lock (dataLock)
				{
					return data;
				}
			}
		}

		
		
		public HttpSession()
		{
			this.ID = Guid.NewGuid().ToString("D");
			this.CreationDate = DateTime.Now;
			this.LastRequest = DateTime.Now;
			this.data = new Dictionary<string, object>();
			
			sessions.Add(this.ID, this);
		}
	}
}
