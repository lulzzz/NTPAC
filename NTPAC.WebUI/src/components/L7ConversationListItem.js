import React from "react";
import PropTypes from "prop-types";
import { Panel } from 'react-bootstrap';

import { L7ConversationApi } from "ntpac_api";
import DateFormatter from "../util/DateFormatter";

import L7PduList from "./L7PduList";

function formatEndPoint({port, address}) {
    return address + ":" + port;
}

function formatProtocolType(protocolType) {
    // Defined by NTPAC.DTO.ConversationTracking.IPProtocolType
    switch (protocolType) {
        case 6:
            return "TCP";
        case 17:
            return "UDP";
        default:
            return protocolType;
    }
}

class L7ConversationListItem extends React.Component {
    constructor(props)  {
        super(props);
        
        this.state = {
            pdus: null
        };

        this.l7Conversation = this.props.l7Conversation;

        this.l7ConversationApi = new L7ConversationApi();

        this.onExpand = this.onExpand.bind(this);
    }

    render() {
        var l7Conversation = this.l7Conversation;
        return (
            <Panel eventKey={l7Conversation.id}>
                <Panel.Heading>
                    <Panel.Title toggle>{formatProtocolType(l7Conversation.protocolType)} {formatEndPoint(l7Conversation.sourceEndPoint)}  {"<-->"}  {formatEndPoint(l7Conversation.destinationEndPoint)} {"//"} {DateFormatter(l7Conversation.firstSeen)} - {DateFormatter(l7Conversation.lastSeen)}  {"//"}  {l7Conversation.pduCount} PDUs</Panel.Title>
                </Panel.Heading>
                <Panel.Collapse onEnter={this.onExpand}>
                    <Panel.Body>
                        { this.state.pdus == null ? (
                            <p>Loading ...</p>
                        ) : (
                            <L7PduList l7Pdus={this.state.pdus}/>
                        )}
                    </Panel.Body>
                </Panel.Collapse>
            </Panel>
        );
    }

    onExpand(e) {
        if (this.state.pdus != null) {
            return;
        }

        this.l7ConversationApi.l7ConversationGet(this.l7Conversation.id, (error, l7ConversationDetail, response) => {
            if (error) {
                console.error(error);
                return;
            }
            const newState = Object.assign({}, this.state, {
                pdus: l7ConversationDetail.pdus
            });
            this.setState(newState);
        });
    }
}


L7ConversationListItem.propTypes = {
    l7Conversation: PropTypes.object.isRequired
};

export default L7ConversationListItem;