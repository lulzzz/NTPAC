import React from "react";
import PropTypes from "prop-types";
import { Table } from 'react-bootstrap';

import CaptureListItem from "./CaptureListItem";

const CaptureList = ( {captures} ) => (
    <div>
        <h3>Captures</h3>
        <Table hover>
            <thead>
                <tr>
                    <th>Capture URI</th>
                    <th>Capture Size (MB)</th>
                    <th>Capture timespan</th>
                    <th>L7 Conversations</th>
                </tr>
            </thead>
            <tbody>
                {captures.map((c, i) => <CaptureListItem key={i} capture={c}/>)}
            </tbody>
        </Table>
    </div>
);  

CaptureList.propTypes = {
    captures: PropTypes.array.isRequired
};

export default CaptureList;