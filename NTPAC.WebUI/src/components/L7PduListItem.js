import React from "react";
import PropTypes from "prop-types";
import hexdump from "hexdump-js"

import DateFormatter from "../util/DateFormatter";
import { ticksToDate } from "../util/DateTickConverter";


function formatDirection(direction) {
    switch (direction) {
        case 1:
            // RIGHTWARDS ARROW
            return "\u2192";
        case 2:
            // LEFTWARDS ARROW
            return "\u2190";
        default:
            return "?";
    }
}

function decodePayload(base64Payload) {
    function charCodeAt(c) {
        return c.charCodeAt(0)
    }   
    var array = new Uint8Array(atob(base64Payload).split('').map(charCodeAt))
    return array.buffer;
}

function L7PduListItem( {l7Pdu} ) {
    var payloadBuffer = decodePayload(l7Pdu.payload);
    return (
        <div>
            {formatDirection(l7Pdu.direction)} {DateFormatter(ticksToDate(l7Pdu.firstSeenTicks))}, {payloadBuffer.byteLength} bytes
            <pre>
             {hexdump(payloadBuffer)}
            </pre>
        </div>
    ); 
}


L7PduListItem.propTypes = {
    l7Pdu: PropTypes.object.isRequired
};

export default L7PduListItem;