﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace VoxelWorld {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Shaders {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Shaders() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("VoxelWorld.Shaders", typeof(Shaders).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to // Compiled shader for all platforms, uncompressed size: 15.0KB
        ///
        ///Shader &quot;Futile/Decal&quot; {
        ///Properties {
        /// _MainTex (&quot;Base (RGB) Trans (A)&quot;, 2D) = &quot;white&quot; {}
        ///}
        ///SubShader { 
        /// Tags { &quot;QUEUE&quot;=&quot;Transparent&quot; &quot;IGNOREPROJECTOR&quot;=&quot;true&quot; &quot;RenderType&quot;=&quot;Transparent&quot; }
        ///
        ///
        /// // Stats for Vertex shader:
        /// //       d3d11 : 9 math
        /// // Stats for Fragment shader:
        /// //       d3d11 : 133 math, 11 texture, 9 branch
        /// Pass {
        ///  Tags { &quot;QUEUE&quot;=&quot;Transparent&quot; &quot;IGNOREPROJECTOR&quot;=&quot;true&quot; &quot;RenderType&quot;=&quot;Transparent&quot; }
        ///  BindChannels {
        ///   Bind &quot;vert [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string Decal {
            get {
                return ResourceManager.GetString("Decal", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to // Compiled shader for all platforms, uncompressed size: 8.6KB
        ///
        ///Shader &quot;Futile/Fog&quot; {
        ///Properties {
        /// _MainTex (&quot;Base (RGB) Trans (A)&quot;, 2D) = &quot;white&quot; {}
        ///}
        ///SubShader { 
        /// Tags { &quot;QUEUE&quot;=&quot;Transparent&quot; &quot;IGNOREPROJECTOR&quot;=&quot;true&quot; &quot;RenderType&quot;=&quot;Transparent&quot; }
        ///
        ///
        /// // Stats for Vertex shader:
        /// //       d3d11 : 9 math
        /// // Stats for Fragment shader:
        /// //       d3d11 : 60 math, 6 texture, 4 branch
        /// Pass {
        ///  Tags { &quot;QUEUE&quot;=&quot;Transparent&quot; &quot;IGNOREPROJECTOR&quot;=&quot;true&quot; &quot;RenderType&quot;=&quot;Transparent&quot; }
        ///  BindChannels {
        ///  [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string Fog {
            get {
                return ResourceManager.GetString("Fog", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to // Compiled shader for all platforms, uncompressed size: 16.1KB
        ///
        ///Shader &quot;Futile/LevelColor&quot; {
        ///Properties {
        /// _MainTex (&quot;Base (RGB) Trans (A)&quot;, 2D) = &quot;white&quot; {}
        ///}
        ///SubShader { 
        /// Tags { &quot;QUEUE&quot;=&quot;Transparent&quot; &quot;IGNOREPROJECTOR&quot;=&quot;true&quot; &quot;RenderType&quot;=&quot;Transparent&quot; }
        /// GrabPass {
        /// }
        ///
        ///
        /// // Stats for Vertex shader:
        /// //       d3d11 : 5 math
        /// // Stats for Fragment shader:
        /// //       d3d11 : 150 math, 18 texture, 9 branch
        /// Pass {
        ///  Tags { &quot;QUEUE&quot;=&quot;Transparent&quot; &quot;IGNOREPROJECTOR&quot;=&quot;true&quot; &quot;RenderType&quot;=&quot;Transparent&quot; }
        ///  BindChan [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string LevelColor {
            get {
                return ResourceManager.GetString("LevelColor", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to // Compiled shader for all platforms, uncompressed size: 9.8KB
        ///
        ///Shader &quot;Futile/VoxelChunk&quot; {
        ///Properties {
        /// _MainTex (&quot;Voxels&quot;, 3D) = &quot;white&quot; {}
        ///}
        ///SubShader { 
        /// LOD 100
        /// Tags { &quot;QUEUE&quot;=&quot;Geometry&quot; &quot;RenderType&quot;=&quot;Voxels&quot; }
        ///
        ///
        /// // Stats for Vertex shader:
        /// //       d3d11 : 10 math
        /// // Stats for Fragment shader:
        /// //       d3d11 : 85 math, 3 branch
        /// Pass {
        ///  Tags { &quot;QUEUE&quot;=&quot;Geometry&quot; &quot;RenderType&quot;=&quot;Voxels&quot; }
        ///  ZTest Less
        ///Program &quot;vp&quot; {
        ///SubProgram &quot;d3d11 &quot; {
        ///// Stats: 10 math
        ///Bind &quot;vertex&quot; Vertex
        ///Bind &quot;color&quot; Color
        ///C [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string VoxelChunk {
            get {
                return ResourceManager.GetString("VoxelChunk", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to // Compiled shader for all platforms, uncompressed size: 9.1KB
        ///
        ///Shader &quot;Futile/VoxelDepth&quot; {
        ///Properties {
        /// _MainTex (&quot;Voxels&quot;, 3D) = &quot;white&quot; {}
        ///}
        ///SubShader { 
        /// LOD 100
        /// Tags { &quot;QUEUE&quot;=&quot;AlphaTest&quot; &quot;RenderType&quot;=&quot;Voxels&quot; }
        ///
        ///
        /// // Stats for Vertex shader:
        /// //       d3d11 : 10 math
        /// // Stats for Fragment shader:
        /// //       d3d11 : 50 math, 3 branch
        /// Pass {
        ///  Tags { &quot;QUEUE&quot;=&quot;AlphaTest&quot; &quot;RenderType&quot;=&quot;Voxels&quot; }
        ///  ZTest Less
        ///Program &quot;vp&quot; {
        ///SubProgram &quot;d3d11 &quot; {
        ///// Stats: 10 math
        ///Bind &quot;vertex&quot; Vertex
        ///Bind &quot;color&quot; Color [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string VoxelDepth {
            get {
                return ResourceManager.GetString("VoxelDepth", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to // Compiled shader for all platforms, uncompressed size: 34.0KB
        ///
        ///Shader &quot;Futile/VoxelLevelColor&quot; {
        ///Properties {
        /// _MainTex (&quot;Base (RGB) Trans (A)&quot;, 3D) = &quot;white&quot; {}
        ///}
        ///SubShader { 
        /// Tags { &quot;QUEUE&quot;=&quot;Transparent&quot; &quot;IGNOREPROJECTOR&quot;=&quot;true&quot; &quot;RenderType&quot;=&quot;Transparent&quot; }
        ///
        ///
        /// // Stats for Vertex shader:
        /// //       d3d11 : 8 math
        /// //        d3d9 : 9 math
        /// //       gles3 : 38 math, 1 texture, 5 branch
        /// //       metal : 2 math
        /// //      opengl : 38 math, 1 texture, 5 branch
        /// // Stats for Fragment shader:
        /// //       d3d11 :  [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string VoxelLevelColor {
            get {
                return ResourceManager.GetString("VoxelLevelColor", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to // Compiled shader for all platforms, uncompressed size: 2.5KB
        ///
        ///Shader &quot;Custom/VoxelLightCookie&quot; {
        ///Properties {
        /// _MainTex (&quot;Base (RGB)&quot;, 2D) = &quot;white&quot; {}
        ///}
        ///SubShader { 
        /// LOD 200
        /// Tags { &quot;RenderType&quot;=&quot;AlphaTest&quot; }
        ///
        ///
        /// // Stats for Vertex shader:
        /// //       d3d11 : 5 math
        /// // Stats for Fragment shader:
        /// //       d3d11 : 2 math
        /// Pass {
        ///  Tags { &quot;RenderType&quot;=&quot;AlphaTest&quot; }
        ///  ZTest Always
        ///Program &quot;vp&quot; {
        ///SubProgram &quot;d3d11 &quot; {
        ///// Stats: 5 math
        ///Bind &quot;texcoord&quot; TexCoord0
        ///ConstBuffer &quot;$Globals&quot; 32
        ///Vector 16 [_MainTex_ST [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string VoxelLightCookie {
            get {
                return ResourceManager.GetString("VoxelLightCookie", resourceCulture);
            }
        }
    }
}
