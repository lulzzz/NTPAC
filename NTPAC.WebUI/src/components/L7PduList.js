import React from "react";
import PropTypes from "prop-types";
import { PanelGroup } from 'react-bootstrap';

import L7PduListItem from "./L7PduListItem";


const L7PduList = ( {l7Pdus} ) => (
    <PanelGroup accordion id="aaaa">
        {l7Pdus.map((l7Pdu, i) => <L7PduListItem key={i} l7Pdu={l7Pdu}/>)}
    </PanelGroup>
);

L7PduList.propTypes = {
    l7Pdus: PropTypes.array.isRequired
};

export default L7PduList;