def secs_to_HHhMMmSSs(total_seconds: int) -> str:
	"""12345 -> '03h25m45s'"""
	sign = "-" if total_seconds < 0 else ""
	s = abs(int(total_seconds))
	h, rem = divmod(s, 3600)
	m, s  = divmod(rem, 60)
	return f"{sign}{h:02}h{m:02}m{s:02}s"