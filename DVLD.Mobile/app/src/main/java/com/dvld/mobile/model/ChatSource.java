package com.dvld.mobile.model;

import com.google.gson.annotations.SerializedName;

public final class ChatSource {
    @SerializedName("documentID") private Long documentID;
    @SerializedName("pageNumber") private Long pageNumber;
    @SerializedName("chunkIndex") private Long chunkIndex;
    @SerializedName("semanticScore") private Double semanticScore;
    @SerializedName("rerankerScore") private Double rerankerScore;
    public Long getDocumentID() { return documentID; }
    public Long getPageNumber() { return pageNumber; }
    public Long getChunkIndex() { return chunkIndex; }
    public Double getSemanticScore() { return semanticScore; }
    public Double getRerankerScore() { return rerankerScore; }
}
