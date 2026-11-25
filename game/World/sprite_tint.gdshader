shader_type canvas_item;

// The current client's tints, applied after the light has darkened the sprite through its modulate colour, and its
// highlight, which doubles the brightness after the tints, and the selected character's green. An outline colour with
// alpha draws the server's item-tier glow: a one-pixel line just outside the sprite, fainter at its corners, darkened by
// the light like the sprite.
uniform bool grey = false;
uniform bool infrared = false;
uniform bool underwater = false;
uniform bool highlight = false;
uniform bool selected = false;
uniform vec4 outline_color = vec4(0.0);

varying vec4 modulate;

const float CORNER_STRENGTH = 2.0 / 3.0;

float opaque_at(sampler2D tex, vec2 uv) {
	if (uv.x < 0.0 || uv.y < 0.0 || uv.x > 1.0 || uv.y > 1.0) {
		return 0.0;
	}
	return step(0.5, texture(tex, uv).a);
}

float outline_strength(sampler2D tex, vec2 uv, vec2 pixel) {
	float side = max(max(opaque_at(tex, uv + vec2(pixel.x, 0.0)), opaque_at(tex, uv - vec2(pixel.x, 0.0))),
		max(opaque_at(tex, uv + vec2(0.0, pixel.y)), opaque_at(tex, uv - vec2(0.0, pixel.y))));
	float corner = max(max(opaque_at(tex, uv + pixel), opaque_at(tex, uv - pixel)),
		max(opaque_at(tex, uv + vec2(pixel.x, -pixel.y)), opaque_at(tex, uv + vec2(-pixel.x, pixel.y))));
	return max(side, corner * CORNER_STRENGTH);
}

void vertex() {
	modulate = COLOR;
}

void fragment() {
	vec4 color = COLOR;
	if (outline_color.a > 0.0 && texture(TEXTURE, UV).a < 0.5) {
		float strength = outline_strength(TEXTURE, UV, TEXTURE_PIXEL_SIZE);
		if (strength > 0.0) {
			color = vec4(outline_color.rgb * modulate.rgb, outline_color.a * strength);
		}
	}
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
	if (highlight) {
		color.rgb *= 2.0;
	}
	if (selected) {
		color.g += 0.5;
	}
	COLOR = color;
}
