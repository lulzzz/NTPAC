function ticksToDate(ticks) {
	// https://ben.lobaugh.net/blog/749/converting-datetime-ticks-to-a-unix-timestamp-and-back-in-php
	var ms = (ticks - 621355968000000000) / 10000000 ;
	var s = ms * 1000;
	return new Date(s);
}

export { ticksToDate };