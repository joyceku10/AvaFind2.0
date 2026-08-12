"""Schema mirrors the columns of the demand extract (sheet "Export").

Source header -> column mapping is kept 1:1 in `import_extract.py`; nothing
here is invented beyond parsing the packed "Role Skills" string into rows.
"""

from datetime import date, datetime

from sqlalchemy import Boolean, Date, DateTime, ForeignKey, Integer, String, Text
from sqlalchemy.orm import DeclarativeBase, Mapped, mapped_column, relationship


class Base(DeclarativeBase):
    pass


class Role(Base):
    __tablename__ = "roles"

    role_id: Mapped[str] = mapped_column(String(20), primary_key=True)
    rfe8: Mapped[str | None] = mapped_column(String(100))          # "RFE 8"
    rfe9: Mapped[str | None] = mapped_column(String(100))          # "RFE 9"
    client: Mapped[str | None] = mapped_column(String(200))
    project_id: Mapped[str | None] = mapped_column(String(20))
    project_name: Mapped[str | None] = mapped_column(String(300))  # "Project"
    role_title: Mapped[str | None] = mapped_column(String(300))
    assigned_role: Mapped[str | None] = mapped_column(String(200))
    primary_contact: Mapped[str | None] = mapped_column(String(200))      # "Role Primary Contact"
    fulfillment_contact: Mapped[str | None] = mapped_column(String(200))  # "Role Fulfillment Contact"
    job_family_group: Mapped[str | None] = mapped_column(String(100))     # "Role Job Family Group"
    role_status: Mapped[str | None] = mapped_column(String(100))
    sold_role: Mapped[bool | None] = mapped_column(Boolean)
    charg_role: Mapped[bool | None] = mapped_column(Boolean)
    channel: Mapped[str | None] = mapped_column(String(100))
    min_level: Mapped[int | None] = mapped_column(Integer)         # "Min Role Level"
    max_level: Mapped[int | None] = mapped_column(Integer)         # "Max Role Level"
    start_date: Mapped[date | None] = mapped_column(Date)          # "Role Start Date"
    end_date: Mapped[date | None] = mapped_column(Date)            # "Role End Date"
    primary_skill_name: Mapped[str | None] = mapped_column(String(200))
    primary_skill_proficiency: Mapped[str | None] = mapped_column(String(50))
    skills_raw: Mapped[str | None] = mapped_column(Text)           # "Role Skills" verbatim
    project_geo: Mapped[str | None] = mapped_column(String(100))
    work_location: Mapped[str | None] = mapped_column(String(100))  # "Role Work Location"
    priority: Mapped[str | None] = mapped_column(String(50))       # "Role Priority"
    created_date: Mapped[date | None] = mapped_column(Date)        # "Role Created Date"
    description: Mapped[str | None] = mapped_column(Text)          # "Role Description"

    skills: Mapped[list["RoleSkill"]] = relationship(
        back_populates="role", cascade="all, delete-orphan", order_by="RoleSkill.position"
    )


class RoleSkill(Base):
    __tablename__ = "role_skills"

    id: Mapped[int] = mapped_column(Integer, primary_key=True, autoincrement=True)
    role_id: Mapped[str] = mapped_column(ForeignKey("roles.role_id", ondelete="CASCADE"))
    position: Mapped[int] = mapped_column(Integer)
    name: Mapped[str] = mapped_column(String(200))
    proficiency: Mapped[str | None] = mapped_column(String(50))    # e.g. "P3 - Advanced"

    role: Mapped[Role] = relationship(back_populates="skills")


class ImportMeta(Base):
    __tablename__ = "import_meta"

    id: Mapped[int] = mapped_column(Integer, primary_key=True)
    source_file: Mapped[str] = mapped_column(String(300))
    extract_date: Mapped[date | None] = mapped_column(Date)
    imported_at: Mapped[datetime] = mapped_column(DateTime)
    row_count: Mapped[int] = mapped_column(Integer)
