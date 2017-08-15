/* Copyright (C) 2017 Kevin Boronka
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

namespace sar.FSM
{
	/// <summary>
	/// Description of CommandQueue.
	/// </summary>
	public class CommandQueue
	{
		private readonly List<Command> commands;
		
		public bool Available { get { return commands.Count > 0; } }
		
		public CommandQueue()
		{
			commands = new List<Command>();
		}
		
		public void QueueCommand(Enum command)
		{
			commands.Add(new Command(command));
		}
		
		public void QueueCommand(Enum command, object paramerter)
		{
			commands.Add(new Command(command, paramerter));
		}

		public Command DequeueCommand()
		{
			if (commands.Count == 0)
			{
				return null;
			}
			
			var currentCommand = commands[commands.Count - 1];
			commands.RemoveAt(commands.Count - 1);
			
			return currentCommand;
		}
	}
}
