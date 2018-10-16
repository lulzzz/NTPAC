import moment from "moment";

export default function(date) {
	return moment(date).format("DD.MM.YYYY HH:mm:ss");
}