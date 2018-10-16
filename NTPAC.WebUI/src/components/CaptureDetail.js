import React from "react";
import { CaptureApi, L7ConversationApi } from "ntpac_api";
import { ListGroup, ListGroupItem } from 'react-bootstrap';

import DateFormatter from "../util/DateFormatter";

import L7ConversationList from "./L7ConversationList";

export default class CaptureDetail extends React.Component {
    constructor(props) {
        super(props)

        this.state = {
            captureId: props.match.params.captureId,
            capture: null
        };

        this.captureApi = new CaptureApi();
        this.l7ConversationApi = new L7ConversationApi();
    }

    render() {
        var capture = this.state.capture;
        if (!capture) {
            return (
                <div>
                    Loading capture detail ...
                </div>
            );
        }

        return (
            <div>
                <h3>Capture detail</h3>
                <ListGroup>
                    <ListGroupItem header="Capture URI (Size)">{capture.uri} ({capture.captureSize} MB)</ListGroupItem>
                    <ListGroupItem header="Timespan">{DateFormatter(capture.firstSeen)} - {DateFormatter(capture.lastSeen)}</ListGroupItem>
                    <ListGroupItem header="Analyzed">{DateFormatter(capture.processed)}</ListGroupItem>
                </ListGroup>

                <L7ConversationList l7Conversations={capture.l7Conversations} />
            </div>
        );
    }

    componentDidMount() {
        this.captureApi.captureGet(this.state.captureId, (error, captureDetail, response) => {
            if (error) {
                console.error(error);
                return;
            }
            //console.log(captureDetail)
            const newState = Object.assign({}, this.state, {
                capture: captureDetail
            });
            this.setState(newState);
        });
    }
}