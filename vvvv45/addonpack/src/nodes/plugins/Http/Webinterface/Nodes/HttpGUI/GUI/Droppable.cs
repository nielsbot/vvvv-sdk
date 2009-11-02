using System;
using System.Collections.Generic;
using System.Text;
using VVVV.PluginInterfaces.V1;
using VVVV.Webinterface.Utilities;
using VVVV.Webinterface.jQuery;

namespace VVVV.Nodes.HttpGUI
{
    class Droppable : JQueryNode, IPlugin, IDisposable
    {
    	
    	#region field declaration 

        private bool FDisposed = false;

        private INodeIn FAcceptNodeInput;
		private IHttpGUIIO FUpstreamAcceptNodeInterface;
		private string FUpstreamAcceptNodeId = null;

        private bool FAcceptNodeInputEventThisFrame;
		
		protected bool FAcceptNodeChangedThisFrame;

        private JavaScriptGenericObject FDroppableArguments;


        #endregion field declaration


        #region constructor/destructor

        /// <summary>
        /// the nodes constructor
        /// nothing to declare for this node
        /// </summary>
        public Droppable()
        {
            FExpression = JQueryExpression.This();
            FDroppableArguments = new JavaScriptGenericObject();
			FExpression.ApplyMethodCall("droppable", FDroppableArguments);
        }

        /// <summary>
        /// Implementing IDisposable's Dispose method.
        /// Do not make this method virtual.
        /// A derived class should not be able to override this method.
        /// </summary>
        /// 

            #region Dispose

            public void Dispose()
            {
                Dispose(true);
                // Take yourself off the Finalization queue
                // to prevent finalization code for this object
                // from executing a second time.
                GC.SuppressFinalize(this);
            }


            /// <summary>
            /// Dispose(bool disposing) executes in two distinct scenarios.
            /// If disposing equals true, the method has been called directly
            /// or indirectly by a user's code. Managed and unmanaged resources
            /// can be disposed.
            /// If disposing equals false, the method has been called by the
            /// runtime from inside the finalizer and you should not reference
            /// other objects. Only unmanaged resources can be disposed.
            /// </summary>
            /// <param name="disposing"></param>
            protected virtual void Dispose(bool disposing)
            {
                // Check to see if Dispose has already been called.
                if (FDisposed == false)
                {
                    if (disposing)
                    {
                        // Dispose managed resources.
                    }
                    // Release unmanaged resources. If disposing is false,
                    // only the following code is executed.
                    //mWebinterfaceSingelton.DeleteNode(mObserver);
                    FHost.Log(TLogType.Message, FPluginInfo.Name.ToString() + "(Http Gui) Node is being deleted");

                    // Note that this is not thread safe.
                    // Another thread could start disposing the object
                    // after the managed resources are disposed,
                    // but before the disposed flag is set to true.
                    // If thread safety is necessary, it must be
                    // implemented by the client.
                }

                FDisposed = true;
            }


            /// <summary>
            /// Use C# destructor syntax for finalization code.
            /// This destructor will run only if the Dispose method
            /// does not get called.
            /// It gives your base class the opportunity to finalize.
            /// Do not provide destructors in WebTypes derived from this class.
            /// </summary>
        ~Droppable()
            {
                // Do not re-create Dispose clean-up code here.
                // Calling Dispose(false) is optimal in terms of
                // readability and maintainability.
                Dispose(false);
            }

            #endregion dispose

        #endregion constructor/destructor


        #region Pugin Information

        public static IPluginInfo FPluginInfo;

        public static IPluginInfo PluginInfo
        {
            get
            {
                if (FPluginInfo == null)
                {
                    //fill out nodes info
                    //see: http://www.vvvv.org/tiki-index.php?page=Conventions.NodeAndPinNaming
                    FPluginInfo = new PluginInfo();

                    //the nodes main name: use CamelCaps and no spaces
                    FPluginInfo.Name = "Droppable";
                    //the nodes category: try to use an existing one
                    FPluginInfo.Category = "JQuery";
                    //the nodes version: optional. leave blank if not
                    //needed to distinguish two nodes of the same name and category
                    FPluginInfo.Version = "";

                    //the nodes author: your sign
                    FPluginInfo.Author = "iceberg";
                    //describe the nodes function
                    FPluginInfo.Help = "Node for adding the JQuery Droppable behavior to an HTML element";
                    //specify a comma separated list of tags that describe the node
                    FPluginInfo.Tags = "";

                    //give credits to thirdparty code used
                    FPluginInfo.Credits = "";
                    //any known problems?
                    FPluginInfo.Bugs = "";
                    //any known usage of the node that may cause troubles?
                    FPluginInfo.Warnings = "";



                    //leave below as is
                    System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace(true);
                    System.Diagnostics.StackFrame sf = st.GetFrame(0);
                    System.Reflection.MethodBase method = sf.GetMethod();
                    FPluginInfo.Namespace = method.DeclaringType.Namespace;
                    FPluginInfo.Class = method.DeclaringType.Name;
                    //leave above as is
                }

                return FPluginInfo;
            }
        }


        #endregion Plugin Information


        #region pin creation

        protected override void OnSetPluginHost()
        {
            // create required pins
            FHost.CreateNodeInput("Accept", TSliceMode.Single, TPinVisibility.True, out FAcceptNodeInput);
			FAcceptNodeInput.SetSubType(new Guid[1] { HttpGUIIO.GUID }, HttpGUIIO.FriendlyName);
        }

        #endregion pin creation


        #region Main Loop



		protected override void OnEvaluate(int SpreadMax, bool changedSpreadSize, string NodeId, List<string> SlideId, bool ReceivedNewString, List<string> ReceivedString)
		{
            bool newDataOnAcceptInputSlice = false;

            if (FAcceptNodeInput.IsConnected && (FAcceptNodeInputEventThisFrame || FUpstreamAcceptNodeInterface.PinIsChanged()))
			{
				newDataOnAcceptInputSlice = true;
				for (int i = 0; i < SpreadMax; i++)
				{
					FUpstreamAcceptNodeId = FUpstreamAcceptNodeInterface.GetNodeId(i);
				}
			}

			FAcceptNodeChangedThisFrame = FAcceptNodeInputEventThisFrame || newDataOnAcceptInputSlice;

            if (changedSpreadSize || DynamicPinsAreChanged())
			{
                for (int i = 0; i < SpreadMax; i++)
                {
                    if (FUpstreamAcceptNodeId != null)
					{
						FDroppableArguments.Set("accept", new ClassSelector(FUpstreamAcceptNodeId));
					}
					else
					{
						FDroppableArguments.Set("accept", Selector.All);
					}

                }
			}

			FAcceptNodeInputEventThisFrame = false;
		}

        #endregion Main Loop

        protected override bool DynamicPinsAreChanged()
		{
			return (FAcceptNodeChangedThisFrame);
		}

        #region IPluginConnections Members

        public override void ConnectPin(IPluginIO pin)
        {
            base.ConnectPin(pin);
            
            //cache a reference to the upstream interface when the NodeInput pin is being connected
            if (pin == FAcceptNodeInput)
			{
				if (FAcceptNodeInput != null)
				{
					INodeIOBase upstreamInterface;
					FAcceptNodeInput.GetUpstreamInterface(out upstreamInterface);
					FUpstreamAcceptNodeInterface = upstreamInterface as IHttpGUIIO;
					FAcceptNodeInputEventThisFrame = true;
				}
			}
        }

        public override void DisconnectPin(IPluginIO pin)
        {
            base.DisconnectPin(pin);
            
            //reset the cached reference to the upstream interface when the NodeInput is being disconnected
            if (pin == FAcceptNodeInput)
			{
				FUpstreamAcceptNodeInterface = null;
				FUpstreamAcceptNodeId = null;
				FAcceptNodeInputEventThisFrame = true;
			}
        }

        #endregion
	}
}
