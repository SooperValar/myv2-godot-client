shader_type canvas_item;

// The current client's magic glow: a crystal shape that fades as the server counts its strength from 1 to 7,
// added on top of what is already drawn.
render_mode blend_add;

uniform vec3 glow_colour = vec3(1.0);
uniform float strength = 1.0;

void fragment() {
	vec2 position = UV * 64.0;
	position.x = (position.x - 32.0) * 1.54 + 32.0;
	position.y = (position.y - 32.0) * 1.09 + 32.0;
	float left = max(0.0, (32.0 - position.x) / 32.0);
	float right = max(0.0, (position.x - 31.0) / 32.0);
	float top = max(0.0, (16.0 - position.y) / 21.0);
	float bottom = max(0.0, (position.y - 55.0) / 23.0) * 2.0;
	float mask = clamp(1.0 - left - right - top - bottom, 0.0, 1.0);
	float glow = clamp(mask / strength, 0.0, 1.0);
	COLOR = vec4(clamp(glow_colour * glow, 0.0, 1.0), 1.0);
}
