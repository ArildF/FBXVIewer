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
out vec3 Position_worldSpace;

out vec3 LightDirection_tangentSpace;
out vec3 EyeDirection_tangentSpace;

void main()
{
    mat4 MVP = P * V * M;
    mat3 MV3x3 = mat3(V * M);
    
    gl_Position = MVP * vec4(vertexPosition_modelspace, 1);
    
    Position_worldSpace = (M * vec4(vertexPosition_modelspace, 1)).xyz;
    
    vec3 vertexPosition_CameraSpace = (V * M * vec4(vertexPosition_modelspace, 1)).xyz;
    vec3 eyeDirection_cameraSpace = vec3(0, 0, 0) - vertexPosition_CameraSpace;
    
    vec3 lightPosition_CameraSpace = (V * vec4(LightPosition_worldSpace, 1)).xyz;
    vec3 lightDirection_cameraSpace = lightPosition_CameraSpace + eyeDirection_cameraSpace;
    
    vec3 vertexTangent_cameraSpace = MV3x3 * tangent_modelSpace;
    vec3 vertexBitangent_cameraSpace = MV3x3 * biTangent_modelSpace;
    vec3 vertexNormal_cameraSpace = MV3x3 * vertexNormal_modelSpace;
    
    mat3 TBN = transpose(mat3(vertexTangent_cameraSpace,
      vertexBitangent_cameraSpace,
      vertexNormal_cameraSpace));
      
    LightDirection_tangentSpace = normalize(TBN * lightDirection_cameraSpace);
    EyeDirection_tangentSpace = normalize(TBN * eyeDirection_cameraSpace);
    
    fragmentColor = biTangent_modelSpace;
    
    UV = uv_uvSpace;
}