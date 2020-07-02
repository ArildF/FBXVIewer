#version 330 
layout(location = 0) in vec3 vertexPosition_modelspace;
layout(location = 1) in vec2 uv_uvSpace;
layout(location = 2) in vec3 vertexNormal_modelSpace;
layout(location = 3) in vec3 tangent_modelSpace;
layout(location = 4) in vec3 biTangent_modelSpace;

uniform mat4 P;
uniform mat4 M;
uniform mat4 V;
uniform vec3 LightPosition_worldSpace;
out vec3 fragmentColor;
out vec2 UV;
out vec3 EyeDirection_cameraSpace;
out vec3 LightDirection_cameraSpace;
out vec3 Normal_cameraSpace;
out vec3 Position_worldSpace;

void main()
{
    mat4 MVP = P * V * M;
    gl_Position = MVP * vec4(vertexPosition_modelspace, 1);
    
    Position_worldSpace = (M * vec4(vertexPosition_modelspace, 1)).xyz;
    
    
    vec3 csPosition = (V * M * vec4(vertexPosition_modelspace, 1)).xyz;    
    EyeDirection_cameraSpace = vec3(0, 0, 0) - csPosition;
    
    vec3 csLightPosition = (V * vec4(LightPosition_worldSpace, 1)).xyz;
    LightDirection_cameraSpace = csLightPosition + EyeDirection_cameraSpace;
    
    Normal_cameraSpace = (V * M * vec4(vertexNormal_modelSpace, 0)).xyz;
    
    fragmentColor = biTangent_modelSpace;
    
    UV = uv_uvSpace;
}