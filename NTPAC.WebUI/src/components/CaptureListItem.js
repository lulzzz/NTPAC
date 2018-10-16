import React from "react";
import PropTypes from "prop-types";
import { withRouter } from "react-router-dom";

import DateFormatter from "../util/DateFormatter";


function CaptureListItem( {capture, history} ) {
    function handleClick(e) {
        history.push("/captures/" + capture.id);
    }

    return (
        <tr onClick={handleClick}>
            <td>{capture.uri}</td>
            <td>{capture.captureSize}</td>
            <td>{DateFormatter(capture.firstSeen)} - {DateFormatter(capture.lastSeen)}</td>
            <td>{capture.l7ConversationCount}</td>
        </tr>
    );
}

CaptureListItem.propTypes = {
    capture: PropTypes.object.isRequired
};

export default withRouter(CaptureListItem);