shader_type canvas_item;

// The current client's tints, applied after the light has darkened the sprite through its modulate colour.
uniform bool grey = false;
uniform bool infrared = false;
uniform bool underwater = false;

void fragment() {
	vec4 color = COLOR;
	if (grey) {
		float average = (color.r + color.g + color.b) / 3.0 * 0.5;
		color.rgb = vec3(average);
	}
	if (infrared) {
		float average = (color.r + color.g + color.b) / 3.0;
		color.rgb = vec3(average, 0.0, 0.0);
	}
	if (underwater) {
		float average = (color.r + color.g + color.b) / 3.0;
		color.r = (color.r + average) / 3.0;
		color.g = (color.g + average) / 2.0;
		color.b = color.b + average * 1.2;
		color.rgb = clamp(color.rgb, 0.0, 1.0);
	}
	COLOR = color;
}
