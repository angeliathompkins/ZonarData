<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
    <xsl:output method="text"/>
    <xsl:strip-space elements="*"/> 

    <xsl:template match="project">
        <xsl:apply-templates select="@*|node()"/>
        <xsl:text>|</xsl:text>
    </xsl:template>

    <xsl:template match="@*">
        <xsl:if test="count(parent::*/@*)=1 and not(parent::project)">
            <xsl:text>;</xsl:text>
        </xsl:if>
        <xsl:value-of select="."/>
        <xsl:if test="parent::project or (last())">
            <xsl:text>;</xsl:text>
        </xsl:if>
    </xsl:template>

    <xsl:template match="*[ancestor::project]">
        <xsl:apply-templates select="@*"/>
        <xsl:if test="preceding-sibling::* and not(@*)">
            <xsl:text>;</xsl:text>
        </xsl:if>
        <xsl:value-of select="."/>
    </xsl:template>

</xsl:stylesheet>
