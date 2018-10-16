import React from "react";
import PropTypes from "prop-types";
import { PanelGroup } from 'react-bootstrap';

import L7ConversationListItem from "./L7ConversationListItem";


const L7ConversationList = ( {l7Conversations} ) => (
    <div>
        <h4>L7 Conversations ({l7Conversations.length})</h4>
        <PanelGroup accordion id="l7conversationListPanels">
            {l7Conversations.map((l7c, i) => <L7ConversationListItem key={i} l7Conversation={l7c}/>)}
        </PanelGroup>
    </div>
);


L7ConversationList.propTypes = {
    l7Conversations: PropTypes.array.isRequired
};

export default L7ConversationList;