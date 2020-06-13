#version 330 core
in vec2 UV;
out vec3 color;

uniform sampler2D diffuseTextureSampler;

void main()
{
  float y = 1.0 - UV.y;
  color = texture(diffuseTextureSampler, vec2(UV.x, y)).rgb;
}