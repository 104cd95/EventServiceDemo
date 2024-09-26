var Lib_WebGLEventHandler = 
{
    WebGLEventHandler_Init: function (onApplicationQuitAction)
    {
        window.addEventListener("beforeunload", function(event) {
            Module['dynCall_v'](onApplicationQuitAction);
	    });
    }
};

mergeInto(LibraryManager.library, Lib_WebGLEventHandler);