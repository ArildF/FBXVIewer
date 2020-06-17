#version 330 
layout(location = 0) in vec3 vertexPosition_modelspace;
layout(location = 1) in vec2 uv_uvSpace;
layout(location = 2) in vec3 vertexNormal_modelSpace;

uniform mat4 P;
uniform mat4 M;
uniform mat4 V;
out vec3 fragmentColor;
out vec2 UV;

void main()
{
    mat4 MVP = P * V * M;
    gl_Position = MVP * vec4(vertexPosition_modelspace, 1);
    fragmentColor = vertexNormal_modelSpace;
    UV = uv_uvSpace;
}